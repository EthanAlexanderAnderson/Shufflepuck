//
//  BillingTransaction.swift
//  BillingServices
//
//  Created by Ayyappa on 27/03/24.
//

import Foundation

@objcMembers
public class BillingTransaction : NSObject  {
    
    public private(set) var identifier: String
    public private(set) var date: Date
    public private(set) var state: BillingTransactionState
    public private(set) var environment: BillingServicesEnvironment
    public private(set) var receipt: String?

    public private(set) var applicationBundleIdentifier: String
    public private(set) var productIdentifier: String
    public private(set) var productType: BillingProductType
    public private(set) var requestedQuantity: Int
    public private(set) var purchasedQuantity: Int
    
    public private(set) var ownershipType: BillingProductOwnershipType
    
    
    public private(set) var  revocationDate: Date?
    public private(set) var  revocationReason: BillingProductRevocationReason
    
    //subscription
    public private(set) var subscriptionStatus: BillingProductSubscriptionStatus?;
    
    public private(set) var purchaseTag: String?
    
    public private(set) var rawData: String?
    
    public private(set) var error: NSError?

    private var finishCallback: (() async -> Void)?
    
    
    
    internal init(identifier: String, date: Date, state: BillingTransactionState, environment: BillingServicesEnvironment, receipt: String?, applicationBundleIdentifier: String, productIdentifier: String, productType: BillingProductType, requestedQuantity: Int, purchasedQuantity: Int, ownershipType: BillingProductOwnershipType, revocationDate: Date? = nil, revocationReason: BillingProductRevocationReason = .none, subscription: BillingProductSubscriptionStatus? = nil, purchaseTag: String? = nil, rawData: String?, error: NSError? = nil, finishCallback:  (() async -> Void)? = nil) {
        self.identifier = identifier
        self.date = date
        self.state = state
        self.environment = environment
        self.receipt = receipt;
        self.applicationBundleIdentifier = applicationBundleIdentifier
        self.productIdentifier = productIdentifier
        self.productType = productType
        self.requestedQuantity = requestedQuantity
        self.purchasedQuantity = purchasedQuantity
        self.ownershipType = ownershipType
        self.revocationDate = revocationDate
        self.revocationReason = revocationReason
        self.subscriptionStatus = subscription
        self.purchaseTag = purchaseTag
        self.rawData = rawData;
        self.error = error
        self.finishCallback = finishCallback
    }
    
    public func finish() {
        Task {
            await self.finishCallback?()
            self.finishCallback = nil
        }
    }
}
