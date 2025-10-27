//
//  BillingProductRenewalState+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 05/04/24.
//

import Foundation
import StoreKit

extension BillingProductRenewalState {
    
    public static func from(_ rawRenewalState: Product.SubscriptionInfo.RenewalState) -> BillingProductRenewalState {
        
        switch rawRenewalState {
            case .subscribed:
                return BillingProductRenewalState.subscribed
            case .expired:
                return BillingProductRenewalState.expired
            case .inBillingRetryPeriod:
                return BillingProductRenewalState.inBillingRetryPeriod
            case .inGracePeriod:
                return BillingProductRenewalState.inGracePeriod
            case .revoked:
                return BillingProductRenewalState.revoked
            default:
                print("Un-implemented renewal state value: \(rawRenewalState)") //WARN
                return BillingProductRenewalState.unknown   
        }
    }
}
