#import <GameKit/GameKit.h>
#import <UIKit/UIKit.h> // Import UIKit for UIViewController

extern "C" {

// Safely strdup a NSString or return fallback
static const char* SafeStrdup(NSString *string, const char *fallback) {
    if (string != nil) {
        const char* utf8Str = [string UTF8String];
        if (utf8Str != NULL) {
            return strdup(utf8Str);
        }
    }
    return strdup(fallback);
}

// Safely strdup a NSURL or return fallback
static const char* SafeStrdupURL(NSURL *url, const char *fallback) {
    if (url != nil) {
        const char* utf8Str = [[url absoluteString] UTF8String];
        if (utf8Str != NULL) {
            return strdup(utf8Str);
        }
    }
    return strdup(fallback);
}

void GameCenterAuthenticatePlayer(void (^completion)(bool, const char*, const char*)) {
    GKLocalPlayer *localPlayer = [GKLocalPlayer localPlayer];

    [localPlayer setAuthenticateHandler:^(UIViewController *viewController, NSError *error) {
        if (viewController) {
            // Present the Game Center login UI
            UIViewController *rootVC = [UIApplication sharedApplication].keyWindow.rootViewController;
            if (rootVC) {
                [rootVC presentViewController:viewController animated:YES completion:nil];
            } else {
                NSLog(@"GameCenterAuthenticatePlayer: Error: Could not get root view controller.");
                if (completion) {
                    completion(false, "CouldNotGetViewController", "");
                }
            }
        } else if (localPlayer.isAuthenticated) {
            NSLog(@"Game Center authenticated: %@", localPlayer.playerID);
            if (completion) {
                completion(true, "Success", SafeStrdup(localPlayer.playerID, "UnknownPlayerID"));
            }
        } else {
            NSLog(@"Game Center authentication failed: %@", error.localizedDescription);
            if (completion) {
                completion(false, "AuthenticationFailed", SafeStrdup(error.localizedDescription, "UnknownError"));
            }
        }
    }];
}

void GetGameCenterPlayerID(void (^completion)(const char*)) {
    if (completion) {
        if ([GKLocalPlayer localPlayer].isAuthenticated) {
            completion(SafeStrdup([GKLocalPlayer localPlayer].playerID, "UnknownPlayerID"));
        } else {
            completion(strdup("UnknownPlayerID"));
        }
    }
}

void GetGameCenterDisplayName(void (^completion)(const char*)) {
    if (completion) {
        if ([GKLocalPlayer localPlayer].isAuthenticated) {
            completion(SafeStrdup([GKLocalPlayer localPlayer].displayName, "UnknownPlayer"));
        } else {
            completion(strdup("UnknownPlayer"));
        }
    }
}

// Fetch identity verification data from Game Center
void FetchGameCenterIdentityVerificationData(void (*callback)(
    const char* publicKeyURL,
    const char* signature,
    const char* salt,
    unsigned long timestamp
)) {
    GKLocalPlayer *localPlayer = [GKLocalPlayer localPlayer];
    if (!localPlayer.isAuthenticated) {
        NSLog(@"Cannot fetch identity data: Player not authenticated");
        if (callback) {
            callback("", "", "", 0);
        }
        return;
    }

    [localPlayer fetchItemsForIdentityVerificationSignatureWithCompletionHandler:
     ^(NSURL *publicKeyURL, NSData *signature, NSData *salt, uint64_t timestamp, NSError *error) {
        if (error) {
            NSLog(@"Failed to fetch identity verification data: %@", error.localizedDescription);
            if (callback) {
                callback("", "", "", 0);
            }
            return;
        }

        const char* publicKeyURLStr = SafeStrdupURL(publicKeyURL, "");
        const char* signatureStr = signature ? strdup([[signature base64EncodedStringWithOptions:0] UTF8String]) : strdup("");
        const char* saltStr = salt ? strdup([[salt base64EncodedStringWithOptions:0] UTF8String]) : strdup("");

        if (callback) {
            callback(publicKeyURLStr, signatureStr, saltStr, (unsigned long)timestamp);
        }
    }];
}

} // extern "C"
