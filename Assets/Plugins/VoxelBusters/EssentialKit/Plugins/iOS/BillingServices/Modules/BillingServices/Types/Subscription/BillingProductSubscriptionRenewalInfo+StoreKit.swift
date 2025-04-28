//
//  BillingProductSubscriptionRenewalInfo+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 05/04/24.
//

import Foundation
import StoreKit

extension BillingProductSubscriptionRenewalInfo {
    
    public static func from(_ id: String, _ purchaseDate: Date, _ rawRenewalState: Product.SubscriptionInfo.RenewalState, _ rawRenewalInfo: Product.SubscriptionInfo.RenewalInfo) -> BillingProductSubscriptionRenewalInfo {
        
        
        return BillingProductSubscriptionRenewalInfo(state: BillingProductRenewalState.from(rawRenewalState),
                                                     applicableOfferIdentifier: rawRenewalInfo.offerID,
                                                     applicableOfferType: BillingProductOfferCategory.from(rawRenewalInfo.offerType),
                                                     lastRenewedDate: purchaseDate,
                                                     lastRenewalId: id,
                                                     isAutoRenewEnabled: rawRenewalInfo.willAutoRenew,
                                                     expirationReason: BillingProductSubscriptionExpirationReason.from(rawRenewalInfo.expirationReason),
                                                     renewalDate: rawRenewalInfo.renewalDate,
                                                     gracePeriodExpirationDate: rawRenewalInfo.gracePeriodExpirationDate,
                                                     priceIncreaseStatus: BillingProductSubscriptionPriceIncreaseStatus.from(rawRenewalInfo.priceIncreaseStatus)
        )
        
    }
    
}
