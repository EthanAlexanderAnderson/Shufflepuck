//
//  IBillingServices.swift
//  BillingServices
//
//  Created by Ayyappa on 24/03/24.
//

import Foundation

public protocol IBillingServices {
    
    // Properties
    var delegate: BillingServicesDelegate? { get }
    var settings: BillingServicesSettings { get }
    
    // Query
    func isAvailable() -> Bool
    func canMakePayments() -> Bool
    func isProductPurchased(_ productId: String) -> Bool
    

    // Init
    func initializeStore(_ productDefinitions: Array<BillingProductDefinition>)
    
    // Purchases
    func buyProduct(_ productId: String,_ options: BuyProductOptions?) -> NSError?
    func restorePurchases(_ forceRefresh: Bool, _ tag: String?);
    
    // Transaction Management
    func getUnfinishedTransactions() -> Array<BillingTransaction>
    func finishTransactions(_ transactions: Array<BillingTransaction>)
    func tryClearingUnfinishedTransactions()
    
    // Helpers
    func getProductWithId(_ id: String) -> BillingProduct?;
    
}
