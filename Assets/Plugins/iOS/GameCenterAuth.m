#import <GameKit/GameKit.h>

extern "C" {
    void AuthenticateGameCenterPlayer() {
        GKLocalPlayer *localPlayer = [GKLocalPlayer localPlayer];
        [localPlayer setAuthenticateHandler:^(UIViewController *viewController, NSError *error) {
            if (viewController) {
                // Present the Game Center login UI
                UIViewController *rootVC = [UIApplication sharedApplication].keyWindow.rootViewController;
                [rootVC presentViewController:viewController animated:YES completion:nil];
            } else if (localPlayer.isAuthenticated) {
                NSLog(@"Game Center authenticated: %@", localPlayer.playerID);
            } else {
                NSLog(@"Game Center authentication failed: %@", error.localizedDescription);
            }
        }];
    }

    const char* GetGameCenterPlayerID() {
        return [[[GKLocalPlayer localPlayer] playerID] UTF8String];
    }

    const char* GetGameCenterTeamID() {
        return [[[GKLocalPlayer localPlayer] teamPlayerID] UTF8String];
    }

    const char* GetGameCenterPublicKeyURL() {
        return [[[GKLocalPlayer localPlayer] publicKeyURL].absoluteString UTF8String];
    }

    const char* GetGameCenterSalt() {
        return [[[GKLocalPlayer localPlayer] salt] UTF8String];
    }

    unsigned long GetGameCenterTimestamp() {
        return (unsigned long)[[GKLocalPlayer localPlayer] timestamp];
    }

    const char* GetGameCenterSignature() {
        return [[[GKLocalPlayer localPlayer] signature] UTF8String];
    }
}
