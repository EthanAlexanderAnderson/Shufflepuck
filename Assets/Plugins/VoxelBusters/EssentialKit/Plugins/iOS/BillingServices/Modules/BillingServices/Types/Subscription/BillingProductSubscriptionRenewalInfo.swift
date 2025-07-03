//
//  BillingProductSubscriptionStatus.swift
//  BillingServices
//
//  Created by Ayyappa on 04/04/24.
//

import Foundation

@objcMembers
public class BillingProductSubscriptionRenewalInfo: NSObject {
    
    public private(set) var state: BillingProductRenewalState
    public private(set) var applicableOfferIdentifier: String?
    public private(set) var applicableOfferType: BillingProductOfferCategory
    public private(set) var lastRenewedDate: Date?
    public private(set) var lastRenewalId: String?
    public private(set) var isAutoRenewEnabled: Bool
    public private(set) var expirationReason: BillingProductSubscriptionExpirationReason
    public private(set) var renewalDate: Date?
    public private(set) var gracePeriodExpirationDate: Date?
    public private(set) var priceIncreaseStatus: BillingProductSubscriptionPriceIncreaseStatus
    
    internal init(state: BillingProductRenewalState, applicableOfferIdentifier: String? = nil, applicableOfferType: BillingProductOfferCategory = BillingProductOfferCategory.unknown, lastRenewedDate: Date?, lastRenewalId: String?, isAutoRenewEnabled: Bool, expirationReason: BillingProductSubscriptionExpirationReason = .none, renewalDate: Date? = nil, gracePeriodExpirationDate: Date? = nil, priceIncreaseStatus: BillingProductSubscriptionPriceIncreaseStatus) {
        self.state = state
        self.applicableOfferIdentifier = applicableOfferIdentifier
        self.applicableOfferType = applicableOfferType
        self.lastRenewedDate = lastRenewedDate
        self.lastRenewalId  = lastRenewalId
        self.isAutoRenewEnabled = isAutoRenewEnabled
        self.expirationReason = expirationReason
        self.renewalDate = renewalDate
        self.gracePeriodExpirationDate = gracePeriodExpirationDate
        self.priceIncreaseStatus = priceIncreaseStatus
    }
}
