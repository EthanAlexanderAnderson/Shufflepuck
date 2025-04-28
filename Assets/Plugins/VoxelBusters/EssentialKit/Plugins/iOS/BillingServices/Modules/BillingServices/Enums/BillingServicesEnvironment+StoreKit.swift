//
//  BillingServicesEnvironment+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 05/04/24.
//

import Foundation
import StoreKit

@available(iOS 16.0, *)
extension BillingServicesEnvironment {
    
    public static func from(_ rawEnvironment: AppStore.Environment) -> BillingServicesEnvironment {
        switch rawEnvironment {
            case .production:
                return BillingServicesEnvironment.production
            case .sandbox:
                return BillingServicesEnvironment.sandbox
            case .xcode:
                return BillingServicesEnvironment.local
            default:
                fatalError("Un-implemented environment value: \(rawEnvironment)")
        }
    }
}
