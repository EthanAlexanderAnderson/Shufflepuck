//
//  BillingProductSubscriptionPriceIncreaseStatus.swift
//  BillingServices
//
//  Created by Ayyappa on 04/04/24.
//

import Foundation

@objc
public enum BillingProductSubscriptionPriceIncreaseStatus: Int {
    case unknown
    case noIncreasePending
    case agreed
    case pending
}
