//
//  BillingProductSubscriptionExpirationReason+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 05/04/24.
//

import Foundation
import StoreKit

extension BillingProductSubscriptionExpirationReason {
    
    public static func from(_ optionalRawExpirationReason: Product.SubscriptionInfo.RenewalInfo.ExpirationReason?) -> BillingProductSubscriptionExpirationReason {
        
        guard let rawExpirationReason = optionalRawExpirationReason else {
            return .none
        }
        
        switch rawExpirationReason {
            case .autoRenewDisabled:
                return BillingProductSubscriptionExpirationReason.autoRenewDisabled
            case .billingError:
                return BillingProductSubscriptionExpirationReason.billingError
            case .didNotConsentToPriceIncrease:
                return BillingProductSubscriptionExpirationReason.didNotConsentToPriceIncrease
            case .productUnavailable:
                return BillingProductSubscriptionExpirationReason.productUnavailable
            case .unknown:
                return BillingProductSubscriptionExpirationReason.unknown
            default:
                print("Un-implemented expiration reason value: \(rawExpirationReason)")//WARN
                return BillingProductSubscriptionExpirationReason.unknown
        }
    }
}

