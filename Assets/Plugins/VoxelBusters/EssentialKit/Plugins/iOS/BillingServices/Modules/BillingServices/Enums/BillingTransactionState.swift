//
//  BillingTransactionState.swift
//  BillingServices
//
//  Created by Ayyappa on 08/05/24.
//

import Foundation

@objc
public enum BillingTransactionState: Int {
    case unknown
    case purchasing
    case purchased
    case failed
    @available(*, deprecated)
    case restored
    case deferred
    case refunded
}
