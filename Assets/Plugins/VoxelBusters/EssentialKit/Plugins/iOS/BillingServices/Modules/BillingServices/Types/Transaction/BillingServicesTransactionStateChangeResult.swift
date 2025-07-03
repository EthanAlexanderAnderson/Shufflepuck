//
//  BillingServicesTransactionStateChangeResult.swift
//  BillingServices
//
//  Created by Ayyappa on 27/03/24.
//

import Foundation

@objcMembers
public class BillingServicesTransactionStateChangeResult : NSObject  {
    public private(set) var transactions: Array<BillingTransaction> = []
    
    public init(transactions: Array<BillingTransaction>) {
        self.transactions = transactions
    }
    
    convenience init(_ transaction: BillingTransaction) {
        self.init(transactions: [transaction])
    }
}

