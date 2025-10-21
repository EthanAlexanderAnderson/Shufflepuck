//
//  BillingProductRevocationReason+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 05/04/24.
//

import Foundation
import StoreKit

extension BillingProductRevocationReason {
    
    public static func from(_ rawRevocationReason: Transaction.RevocationReason?) -> BillingProductRevocationReason {
        
        if rawRevocationReason == nil {
            return BillingProductRevocationReason.none
        }
                
        switch rawRevocationReason {
            case Transaction.RevocationReason.developerIssue:
                return .developerIssue
            case Transaction.RevocationReason.other:
                return .unknown
            default:
                print("***Un-implemented revocation reason value: \(String(describing: rawRevocationReason?.rawValue))***")
                return .unknown
        }
    }
}
