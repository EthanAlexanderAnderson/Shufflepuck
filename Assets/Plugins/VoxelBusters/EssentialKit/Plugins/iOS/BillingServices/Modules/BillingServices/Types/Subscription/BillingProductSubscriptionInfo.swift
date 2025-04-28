//
//  BillingProductSubscriptionDefinition.swift
//  BillingServices
//
//  Created by Ayyappa on 04/04/24.
//

import Foundation

@objcMembers
public class BillingProductSubscriptionInfo: NSObject {
    
    public private(set) var groupIdentifier: String;
    public private(set) var groupDisplayName: String;
    public private(set) var level: Int; //groupLevel
    
    public private(set) var period: BillingPeriod
    
    public private(set) var isEligibleForIntroductoryOffer: Bool
    
    internal init(groupIdentifier: String, groupDisplayName: String, level: Int, period: BillingPeriod, isEligibleForIntroductoryOffer: Bool) {
        self.groupIdentifier = groupIdentifier
        self.groupDisplayName = groupDisplayName
        self.level = level
        self.period = period
        self.isEligibleForIntroductoryOffer = isEligibleForIntroductoryOffer
    }
}
