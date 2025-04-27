#import <GameKit/GameKit.h>
#import <UIKit/UIKit.h> // Import UIKit for UIViewController
#import <os/log.h> // For potentially better logging (optional)

// Define a log category (optional, but good practice)
static os_log_t gameCenterLog() {
    static dispatch_once_t onceToken;
    static os_log_t log;
    dispatch_once(&onceToken, ^{
        log = os_log_create("com.yourcompany.yourgame", "GameCenterAuth"); // Replace with your bundle ID/subsystem
    });
    return log;
}


extern "C" {

// Safely strdup a NSString or return fallback.
// Caller MUST free the returned C string using free().
static const char* SafeStrdup(NSString *string, const char *fallback) {
    if (string != nil) {
        const char* utf8Str = [string UTF8String];
        if (utf8Str != NULL) {
            // Use strdup which allocates memory that must be freed by the caller.
            return strdup(utf8Str);
        }
    }
    // Use strdup for the fallback as well to maintain consistent memory management responsibility.
    return strdup(fallback);
}

// Safely strdup a NSURL's absolute string or return fallback.
// Caller MUST free the returned C string using free().
static const char* SafeStrdupURL(NSURL *url, const char *fallback) {
    if (url != nil) {
        // Get the absolute string representation of the URL.
        NSString *absoluteString = [url absoluteString];
        const char* utf8Str = [absoluteString UTF8String];
        if (utf8Str != NULL) {
            // Use strdup which allocates memory that must be freed by the caller.
            return strdup(utf8Str);
        }
    }
    // Use strdup for the fallback as well.
    return strdup(fallback);
}

// Helper function to get the current key window or the first active window scene's window.
static UIWindow* GetCurrentWindow() {
     if (@available(iOS 13.0, *)) {
         // Iterate over connected scenes to find the active one.
         for (UIScene *scene in [UIApplication sharedApplication].connectedScenes) {
             if ([scene isKindOfClass:[UIWindowScene class]] && scene.activationState == UISceneActivationStateForegroundActive) {
                 // Return the first window of the active window scene.
                 // Note: Apps can have multiple windows; this gets the first one.
                 UIWindowScene *windowScene = (UIWindowScene *)scene;
                 for (UIWindow *window in windowScene.windows) {
                    // Sometimes keyWindow property might be more reliable even in iOS 13+
                    if (window.isKeyWindow) {
                        return window;
                    }
                 }
                 // Fallback to the first window if no key window found in the scene
                 return windowScene.windows.firstObject;
             }
         }
     } else {
         // Fallback for iOS versions prior to 13.
         #pragma clang diagnostic push
         #pragma clang diagnostic ignored "-Wdeprecated-declarations"
         // keyWindow is deprecated but necessary for older versions.
         return [UIApplication sharedApplication].keyWindow;
         #pragma clang diagnostic pop
     }
     // Return nil if no suitable window is found.
     os_log_error(gameCenterLog(), "Could not find a suitable UIWindow.");
     return nil;
}

// Authenticates the local Game Center player.
// Calls the completion handler with success status, status message, and player ID.
// The caller MUST free the returned C strings (status message and player ID) using a C free() function.
void GameCenterAuthenticatePlayer(void (^completion)(bool success, const char* statusMessage, const char* playerID)) {
    GKLocalPlayer *localPlayer = [GKLocalPlayer localPlayer];
    // Use __weak to prevent a retain cycle within the authentication handler block.
    __weak GKLocalPlayer *weakPlayer = localPlayer;

    [localPlayer setAuthenticateHandler:^(UIViewController *viewController, NSError *error) {
        // Create a strong reference to the weak player inside the block.
        // This ensures the player object stays alive during the block's execution,
        // but doesn't prolong its life beyond that (avoiding the cycle).
        GKLocalPlayer *strongPlayer = weakPlayer;

        // Check if the player object still exists. It might have been deallocated
        // if the context where GameCenterAuthenticatePlayer was called disappeared.
        if (!strongPlayer) {
            os_log_error(gameCenterLog(), "GameCenterAuthenticatePlayer: Player reference lost before handler executed.");
            if (completion) {
                // Provide consistent memory management: strdup the static strings too.
                completion(false, strdup("InternalError"), strdup(""));
            }
            return; // Exit the block
        }

        // Scenario 1: Game Center needs user interaction (login/approval UI).
        if (viewController) {
            os_log_info(gameCenterLog(), "Game Center authentication requires user interaction. Presenting view controller.");
            UIWindow *currentWindow = GetCurrentWindow();
            if (currentWindow) {
                UIViewController *rootVC = currentWindow.rootViewController;
                // Navigate up the presentation stack to find the topmost view controller.
                while (rootVC.presentedViewController) {
                    rootVC = rootVC.presentedViewController;
                }

                if (rootVC) {
                    [rootVC presentViewController:viewController animated:YES completion:^{
                         os_log_info(gameCenterLog(), "Game Center view controller presented.");
                         // NOTE: The completion handler MAY be called AGAIN by GameKit AFTER
                         // the view controller is dismissed by the user (with either success or failure).
                         // Therefore, we typically DON'T call the C# completion handler here,
                         // but wait for the subsequent call from GameKit that indicates the final status.
                         // If GameKit *doesn't* call the handler again after presentation,
                         // this function's C# completion might never be called in this path,
                         // which could be a problem depending on expected behavior.
                         // Consider adding timeout logic on the C# side if necessary.
                    }];
                } else {
                    os_log_error(gameCenterLog(), "GameCenterAuthenticatePlayer: Error: Could not get root view controller.");
                    if (completion) {
                        completion(false, strdup("CouldNotGetViewController"), strdup(""));
                    }
                }
            } else {
                os_log_error(gameCenterLog(), "GameCenterAuthenticatePlayer: Error: Could not get current window.");
                if (completion) {
                    completion(false, strdup("CouldNotGetWindow"), strdup(""));
                }
            }
        }
        // Scenario 2: Player is already authenticated.
        else if (strongPlayer.isAuthenticated) {
            os_log_info(gameCenterLog(), "Game Center player already authenticated: %@", strongPlayer.gamePlayerID);
            if (completion) {
                // Use SafeStrdup which handles potential nil playerID and returns a string requiring free().
                completion(true, strdup("Success"), SafeStrdup(strongPlayer.gamePlayerID, "UnknownPlayerID"));
            }
        }
        // Scenario 3: Authentication failed or was cancelled by the user.
        else {
            NSString *errorDesc = error ? error.localizedDescription : @"Unknown authentication error or user cancelled.";
            // Check for specific error codes if needed (e.g., user cancelled)
            if (error && [error.domain isEqualToString:GKErrorDomain] && error.code == GKErrorCancelled) {
                 os_log_info(gameCenterLog(), "Game Center authentication cancelled by user.");
                 errorDesc = @"UserCancelled"; // Provide a specific code for cancellation
            } else {
                 os_log_error(gameCenterLog(), "Game Center authentication failed: %@", errorDesc);
            }

            if (completion) {
                 // Return the specific error or a generic failure message.
                completion(false, SafeStrdup(errorDesc, "AuthenticationFailed"), strdup(""));
            }
        }
    }];
}

// Gets the Game Center Player ID if authenticated.
// Caller MUST free the returned C string using free().
void GetGameCenterPlayerID(void (^completion)(const char* playerID)) {
    if (completion) {
        GKLocalPlayer *localPlayer = [GKLocalPlayer localPlayer];
        if (localPlayer.isAuthenticated) {
            os_log_info(gameCenterLog(), "Returning authenticated player ID: %@", localPlayer.gamePlayerID);
            completion(SafeStrdup(localPlayer.gamePlayerID, "UnknownPlayerID"));
        } else {
            os_log_warning(gameCenterLog(), "Player not authenticated. Returning default player ID.");
            completion(strdup("UnknownPlayerID")); // Ensure caller always needs to free
        }
    }
}

// Gets the Game Center Display Name if authenticated.
// Caller MUST free the returned C string using free().
void GetGameCenterDisplayName(void (^completion)(const char* displayName)) {
    if (completion) {
        GKLocalPlayer *localPlayer = [GKLocalPlayer localPlayer];
        if (localPlayer.isAuthenticated) {
             os_log_info(gameCenterLog(), "Returning authenticated player display name: %@", localPlayer.displayName);
            completion(SafeStrdup(localPlayer.displayName, "UnknownPlayer"));
        } else {
             os_log_warning(gameCenterLog(), "Player not authenticated. Returning default display name.");
            completion(strdup("UnknownPlayer")); // Ensure caller always needs to free
        }
    }
}

// Fetches identity verification data from Game Center.
// Requires iOS 14.0+ / macOS 11.0+.
// Calls the callback with URL, signature, salt, and timestamp.
// The caller MUST free the returned C strings (publicKeyURL, signature, salt) using free().
void FetchGameCenterIdentityVerificationData(void (*callback)(
    const char* publicKeyURL, // Needs free()
    const char* signature,    // Needs free()
    const char* salt,         // Needs free()
    unsigned long timestamp
)) {
    GKLocalPlayer *localPlayer = [GKLocalPlayer localPlayer];

    // Ensure the player is authenticated first.
    if (!localPlayer.isAuthenticated) {
        os_log_error(gameCenterLog(), "Cannot fetch identity data: Player not authenticated");
        if (callback) {
            // Return empty strings (allocated via strdup for consistent memory management).
            callback(strdup(""), strdup(""), strdup(""), 0);
        }
        return;
    }

    // Check if the API is available on the current OS version.
    // Even if deployment target is 14.0, this check makes the code more robust.
    if (@available(iOS 14.0, macOS 11.0, *)) { // Added macOS 11 check for completeness
        os_log_info(gameCenterLog(), "Fetching identity verification signature...");
        [localPlayer fetchItemsForIdentityVerificationSignatureWithCompletionHandler:
         ^(NSURL *publicKeyURL, NSData *signatureData, NSData *saltData, uint64_t timestamp, NSError *error) {
            if (error) {
                os_log_error(gameCenterLog(), "Failed to fetch identity verification data: %@", error.localizedDescription);
                if (callback) {
                    // Return empty strings on error.
                    callback(strdup(""), strdup(""), strdup(""), 0);
                }
                return;
            }

            os_log_info(gameCenterLog(), "Successfully fetched identity verification data.");

            // Safely convert URL, signature, and salt to C strings using strdup.
            // The caller is responsible for freeing these strings.
            const char* publicKeyURLStr = SafeStrdupURL(publicKeyURL, "");
            const char* signatureStr = (signatureData) ? strdup([[signatureData base64EncodedStringWithOptions:0] UTF8String]) : strdup("");
            const char* saltStr = (saltData) ? strdup([[saltData base64EncodedStringWithOptions:0] UTF8String]) : strdup("");

            if (callback) {
                callback(publicKeyURLStr, signatureStr, saltStr, (unsigned long)timestamp);
            } else {
                // If callback is null, we need to free the allocated memory here to prevent leaks.
                 os_log_warning(gameCenterLog(), "Callback was null, freeing allocated strings locally.");
                 free((void*)publicKeyURLStr);
                 free((void*)signatureStr);
                 free((void*)saltStr);
            }
        }];
    } else {
        // API not available on this OS version.
        os_log_error(gameCenterLog(), "Identity verification data API is not available on this OS version (requires iOS 14.0+ / macOS 11.0+).");
        if (callback) {
            // Indicate unavailability by returning empty strings.
            callback(strdup(""), strdup(""), strdup(""), 0);
        }
    }
}

// ** IMPORTANT C FUNCTION FOR MEMORY MANAGEMENT **
// Export this function so the calling environment (e.g., C#) can call it
// to free the strings allocated by SafeStrdup, SafeStrdupURL, and strdup.
void FreeGameCenterString(const char* ptr) {
    if (ptr != NULL) {
        free((void*)ptr);
    }
}


} // extern "C"