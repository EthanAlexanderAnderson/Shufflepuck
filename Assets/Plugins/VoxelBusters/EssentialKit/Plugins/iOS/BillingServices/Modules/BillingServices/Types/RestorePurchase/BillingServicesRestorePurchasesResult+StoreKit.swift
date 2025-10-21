//
//  BillingServicesRestorePurchasesResult+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 28/03/24.
//

import Foundation
import StoreKit
import VBCoreLibrary

extension BillingServicesRestorePurchasesResult {
    public static func from(with rawTransactions: Transaction.Transactions, for tag: String?) async -> BillingServicesRestorePurchasesResult {
        
        var transactions: Array<BillingTransaction> = await BillingTransaction.from(rawTransactions)
                
        if !tag.isNullOrEmpty() {
            transactions = transactions.filter({ $0.purchaseTag == tag })
        }
                                
        return BillingServicesRestorePurchasesResult(transactions: transactions)
    }
}
