//
//  BillingProductType+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 05/04/24.
//

import Foundation
import StoreKit

extension BillingProductType {
    public static func from(_ rawProductType: Product.ProductType) -> BillingProductType {
        switch rawProductType {
                
            case .consumable:
                return BillingProductType.consumable
            case .nonConsumable:
                return BillingProductType.nonConsumable
            case .nonRenewable:
                return BillingProductType.nonRenewable
            case .autoRenewable:
                return BillingProductType.autoRenewable
            
            default:
                fatalError("Un-implemented product type value: \(rawProductType)")
        }
        
    }
}
