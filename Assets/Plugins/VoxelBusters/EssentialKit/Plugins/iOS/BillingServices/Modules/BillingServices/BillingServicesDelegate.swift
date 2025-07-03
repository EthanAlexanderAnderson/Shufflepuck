//
//  BillingServicesDelegate.swift
//  BillingServices
//
//  Created by Ayyappa on 24/03/24.
//

import Foundation



@objc public protocol BillingServicesDelegate : NSObjectProtocol {
    
    @objc optional func didInitializeStoreComplete(result: BillingServicesInitializeStoreResult, error: NSError?)
    @objc optional func didTransactionStateChange(result: BillingServicesTransactionStateChangeResult)
    @objc optional func didRestorePurchasesComplete(result: BillingServicesRestorePurchasesResult, error: NSError?)
}
