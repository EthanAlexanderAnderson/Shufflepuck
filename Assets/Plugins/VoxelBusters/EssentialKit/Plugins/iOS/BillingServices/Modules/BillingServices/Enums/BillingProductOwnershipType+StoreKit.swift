//
//  BillingProductOwnershipType+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 05/04/24.
//

import Foundation
import StoreKit

extension BillingProductOwnershipType {
    
    public static func from(_ rawOwnershipType: Transaction.OwnershipType) -> BillingProductOwnershipType {
        
        switch rawOwnershipType {
            case .purchased:
                return BillingProductOwnershipType.buyer
            case .familyShared:
                return BillingProductOwnershipType.familyShared
            default:
                fatalError("Un-implemented ownership type value: \(rawOwnershipType)")
        }
    }
}
