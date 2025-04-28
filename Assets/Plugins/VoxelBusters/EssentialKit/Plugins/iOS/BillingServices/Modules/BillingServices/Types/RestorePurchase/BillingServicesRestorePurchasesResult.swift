//
//  BillingServicesRestorePurchasesResult.swift
//  BillingServices
//
//  Created by Ayyappa on 27/03/24.
//

import Foundation

@objcMembers
public class BillingServicesRestorePurchasesResult : NSObject  {
    public private(set) var transactions: Array<BillingTransaction>? = nil
    
    public init(transactions: Array<BillingTransaction>?) {
        self.transactions = transactions
    }
}
