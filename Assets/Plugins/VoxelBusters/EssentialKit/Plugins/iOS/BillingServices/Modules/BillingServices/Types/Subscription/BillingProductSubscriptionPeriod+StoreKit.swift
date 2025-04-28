//
//  BillingProductSubscriptionPeriod+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 05/04/24.
//

import Foundation
import StoreKit

extension BillingPeriod {
    
    public static func from(_ rawPeriod: Product.SubscriptionPeriod) ->  BillingPeriod {
        
        return BillingPeriod(duration: Double(rawPeriod.value), unit: BillingPeriodUnit.from(rawUnit:rawPeriod.unit))
        
    }
}
