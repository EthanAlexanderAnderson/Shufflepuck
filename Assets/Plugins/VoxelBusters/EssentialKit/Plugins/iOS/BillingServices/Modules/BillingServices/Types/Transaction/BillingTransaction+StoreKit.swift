//
//  BillingTransaction+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 27/03/24.
//

import Foundation
import StoreKit


extension BillingTransaction {

    public static func from(_ rawTransaction: Transaction, _ jwsReceipt: String?) async -> BillingTransaction {
        
        let subscriptionStatus = await  BillingProductSubscriptionStatus.from(rawTransaction)

        var environment: BillingServicesEnvironment = BillingServicesEnvironment.unknown
        if #available(iOS 16.0, *) {
            environment = BillingServicesEnvironment.from(rawTransaction.environment)
        }
        
        return BillingTransaction(identifier: String(rawTransaction.originalID),
                                  date: rawTransaction.purchaseDate,
                                  state: BillingTransactionState.purchased,
                                  environment: environment,
                                  receipt: jwsReceipt,
                                  applicationBundleIdentifier: rawTransaction.appBundleID,
                                  productIdentifier: rawTransaction.productID,
                                  productType: BillingProductType.from(rawTransaction.productType),
                                  requestedQuantity: rawTransaction.purchasedQuantity, //Both requested and purchased quantities are same if its a successful transaction.
                                  purchasedQuantity: rawTransaction.purchasedQuantity,
                                  ownershipType: BillingProductOwnershipType.from(rawTransaction.ownershipType),
                                  revocationDate: rawTransaction.revocationDate,
                                  revocationReason: BillingProductRevocationReason.from(rawTransaction.revocationReason),
                                  subscription: subscriptionStatus,
                                  purchaseTag: rawTransaction.appAccountToken?.uuidString,
                                  rawData: ["transaction" : String(decoding: rawTransaction.jsonRepresentation, as: UTF8.self)].toJson(),
                                  error: nil,
                                  finishCallback:  { await rawTransaction.finish() })
    }
    
    public static func from(_ rawTransactions: Transaction.Transactions) async -> Array<BillingTransaction> {
        var transactions: Array<BillingTransaction> = []
                
        for await result in rawTransactions {
            guard case .verified(let rawTransaction) = result else {
                continue
            }
            transactions.append(await BillingTransaction.from(rawTransaction, result.jwsRepresentation))
        }
        return transactions;
    }
    
    public static func fromError(_ product: Product, _ buyOptions: BuyProductOptions, _ error: NSError) -> BillingTransaction {
        
        let subscriptionStatus: BillingProductSubscriptionStatus? = nil

        
        return BillingTransaction(identifier: "",
                                  date: Date(timeIntervalSince1970: 0),
                                  state: BillingTransactionState.failed,
                                  environment: BillingServicesEnvironment.unknown,
                                  receipt: nil,
                                  applicationBundleIdentifier: Bundle.main.bundleIdentifier ?? "",
                                  productIdentifier: product.id,
                                  productType: BillingProductType.from(product.type),
                                  requestedQuantity: buyOptions.quantity,
                                  purchasedQuantity: 0,
                                  ownershipType: BillingProductOwnershipType.none,
                                  revocationDate: nil,
                                  revocationReason: .none,
                                  subscription: subscriptionStatus,
                                  purchaseTag: buyOptions.tag?.uuidString,
                                  rawData: nil,
                                  error: error,
                                  finishCallback: nil
                                  )
    }
}
