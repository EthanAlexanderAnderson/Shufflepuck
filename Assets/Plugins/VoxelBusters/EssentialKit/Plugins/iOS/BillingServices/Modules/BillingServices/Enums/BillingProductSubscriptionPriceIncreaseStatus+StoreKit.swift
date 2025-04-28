//
//  BillingProductSubscriptionPriceIncreaseStatus+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 05/04/24.
//

import Foundation
import StoreKit

extension BillingProductSubscriptionPriceIncreaseStatus {
    
    public static func from(_ rawPriceIncreaseStatus: Product.SubscriptionInfo.RenewalInfo.PriceIncreaseStatus) -> BillingProductSubscriptionPriceIncreaseStatus {
        switch rawPriceIncreaseStatus {
            case .agreed:
                return BillingProductSubscriptionPriceIncreaseStatus.agreed
            case .pending:
                return BillingProductSubscriptionPriceIncreaseStatus.pending
            case .noIncreasePending:
                return BillingProductSubscriptionPriceIncreaseStatus.noIncreasePending
        }
    }
}
