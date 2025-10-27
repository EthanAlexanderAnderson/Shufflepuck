//
//  BillingProductSubscriptionPeriodUnit+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 05/04/24.
//

import Foundation
import StoreKit

extension BillingPeriodUnit {
    
    public static func from(rawUnit: Product.SubscriptionPeriod.Unit) -> BillingPeriodUnit {
        switch rawUnit {
            case .day:
                return .day
            case .week:
                return .week
            case .month:
                return .month
            case .year:
                return .year
            default:
                fatalError("Un-implemented for period unit value: \(rawUnit)");
        }
    }
}
