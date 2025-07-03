//
//  BillingProductRenewalState.swift
//  BillingServices
//
//  Created by Ayyappa on 04/04/24.
//

import Foundation

@objc
public enum BillingProductRenewalState: Int {
    case unknown
    case subscribed //Active
    case expired
    case inBillingRetryPeriod
    case inGracePeriod
    case revoked
}
