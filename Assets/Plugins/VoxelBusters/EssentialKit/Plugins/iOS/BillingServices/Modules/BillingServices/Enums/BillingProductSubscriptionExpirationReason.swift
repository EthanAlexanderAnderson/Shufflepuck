//
//  BillingProductSubscriptionExpirationReason.swift
//  BillingServices
//
//  Created by Ayyappa on 04/04/24.
//

import Foundation

@objc
public enum BillingProductSubscriptionExpirationReason: Int {
    case none
    case unknown
    case autoRenewDisabled
    case billingError
    case didNotConsentToPriceIncrease
    case productUnavailable
}
