//
//  BillingServicesInitializeStoreResult.swift
//  BillingServices
//
//  Created by Ayyappa on 26/03/24.
//

import Foundation

@objcMembers
public class BillingServicesInitializeStoreResult : NSObject  {
    public private(set) var products: Array<BillingProduct> = []
    public private(set) var invalidProductDefinitions: Array<BillingProductDefinition> = []

    public init(products: Array<BillingProduct>, invalidProductDefinitions: Array<BillingProductDefinition>) {
        self.products = products
        self.invalidProductDefinitions = invalidProductDefinitions
    }
}


