//
//  BillingProductSubscriptionInfo+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 05/04/24.
//

import Foundation
import StoreKit

extension BillingProductSubscriptionInfo {
    

    
    public static func from(_ rawProduct: Product) async -> BillingProductSubscriptionInfo? {
        
        guard let subscription = rawProduct.subscription else {
            return nil
        };
        
        
        let isEligibleForIntroOffer = await subscription.isEligibleForIntroOffer
        
        let info = BillingProductSubscriptionInfo(  groupIdentifier: subscription.subscriptionGroupID,
                                                    groupDisplayName: getGroupDisplayName(subscription),
                                                    level: getGroupLevel(subscription),
                                                    period: BillingPeriod.from(subscription.subscriptionPeriod),
                                                    isEligibleForIntroductoryOffer: isEligibleForIntroOffer)
        
        
        return info
        
    }
    
    fileprivate static func getGroupDisplayName(_ subscription: Product.SubscriptionInfo) -> String {
        if #available(iOS 16.4, *) {
            return subscription.groupDisplayName
        } else {
            return ""
        }
    }
    
    fileprivate static func getGroupLevel(_ subscription: Product.SubscriptionInfo) -> Int {
        if #available(iOS 16.4, *) {
            return subscription.groupLevel
        } else {
            return -1
        }
    }
}
