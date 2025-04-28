//
//  BillingProductSubscriptionInfo.swift
//  BillingServices
//
//  Created by Ayyappa on 04/04/24.
//

import Foundation

@objcMembers
public class BillingProductSubscriptionStatus : NSObject {

    public private(set) var groupIdentifier: String
    public private(set) var renewalInfo: BillingProductSubscriptionRenewalInfo?
    public private(set) var expirationDate: Date?;
    public private(set) var isUpgraded: Bool;
    public private(set) var appliedOfferIdentifier: String?;
    public private(set) var appliedOfferType: BillingProductOfferCategory;
    
    internal init(groupIdentifier: String, renewalInfo: BillingProductSubscriptionRenewalInfo? = nil, expirationDate: Date? = nil, isUpgraded: Bool, appliedOfferIdentifier: String? = nil, appliedOfferType: BillingProductOfferCategory = BillingProductOfferCategory.unknown) {
        self.groupIdentifier = groupIdentifier
        self.renewalInfo = renewalInfo
        self.expirationDate = expirationDate
        self.isUpgraded = isUpgraded
        self.appliedOfferIdentifier = appliedOfferIdentifier
        self.appliedOfferType = appliedOfferType
    }
}
