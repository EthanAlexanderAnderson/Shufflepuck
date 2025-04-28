//
//  EKBillingServices.swift
//  BillingServices
//
//  Created by Ayyappa on 23/03/24.
//

import Foundation
import os
import StoreKit
import VBCoreLibrary

@objcMembers
public class BillingServicesStoreKitImplementation : NSObject, IBillingServices {

    //MARK: - Public Getters
    public var settings: BillingServicesSettings
    public var delegate: BillingServicesDelegate?
    
    //MARK: - Private
    private var currentPurchases: Array<String>?
    private var rawProducts: Array<Product> = []
    private var products: Array<BillingProduct>?
    private var transactionUpdatesListenerTask: Task<Void, Never>? = nil
    private var purchaseIntentsListenerTask: Task<Void, Never>? = nil
    
    //MARK: Static
    private static let logger = Logger(
            subsystem: Bundle.main.bundleIdentifier!,
            category: String(describing: BillingServicesStoreKitImplementation.self)
        )

    //MARK: - Lifecycle
    public init(settings: BillingServicesSettings, delegate: BillingServicesDelegate? = nil) {
        self.settings = settings
        self.delegate = delegate
        super.init()
        
        transactionUpdatesListenerTask  =   createTransactionUpdatesListenerTask()
        purchaseIntentsListenerTask     =   createPurchaseIntentsListenerTask()
    }
    
    deinit {
        transactionUpdatesListenerTask?.cancel()
        purchaseIntentsListenerTask?.cancel()
    }
    
    //MARK: - Querying
    public func isAvailable() -> Bool {
        return true;
    }
    
    public func canMakePayments() -> Bool {
        return AppStore.canMakePayments
    }
    
    public func isProductPurchased(_ productId: String) -> Bool {
        
        var isPurchased: Bool = false
        
        if(self.isStoreInitialized()) {
            Task.runSyncronously { await self.updateCurrentPurchases() }
            isPurchased = self.currentPurchases!.contains(productId);
        }
        
        return isPurchased
    }
    
    //MARK: - Store Setup
    public func initializeStore(_ productDefinitions: Array<BillingProductDefinition>) {
        Task { @MainActor in
            await self.initializeStoreAsync(productDefinitions);
        }
    }
    
    //MARK: - Purchases
    public func buyProduct(_ productId: String,_ options: BuyProductOptions?) -> NSError? {
        
        if(!isStoreInitialized()) {
            return BillingServicesError.buyProductFailed(code: BillingServicesErrorCode.storeNotInitialized, reason: "Store not initialized. Make sure you initialize store before accessing any other api.") as NSError
        }
        
        if let product = rawProducts.first(where: {$0.id == productId}) {
            Task {
                await self.buyProductAsync(product,options ?? BuyProductOptions())
            }
        } else {
            return BillingServicesError.buyProductFailed(code: BillingServicesErrorCode.productNotAvailable, reason: "Provided productId - \(productId) is unavailable.") as NSError
        }
        
        return nil
    }
    
    // ForceRefresh should be set to  for a manual user action only as it triggers an authentication dialog.
    public func restorePurchases(_ forceRefresh: Bool,_ tag: String?) {
        
        Task {
            var error: BillingServicesError? = nil
            var result: BillingServicesRestorePurchasesResult? = nil
            
            if(!isStoreInitialized()) {
                error = BillingServicesError.restorePurchasesFailed(code: BillingServicesErrorCode.storeNotInitialized, reason: "Store not initialized. Make sure you initialize store before accessing any other api.")
            } else {
                
                do {
                    if(forceRefresh) {
                        try await AppStore.sync()
                    }
                    result = await BillingServicesRestorePurchasesResult.from(with: Transaction.currentEntitlements, for: tag)
                } catch let localError {
                    error = BillingServicesError.restorePurchasesFailed(code: BillingServicesErrorCode.unknown, reason: localError.localizedDescription)
                }
                
                print("Current Entitlements : \(Transaction.currentEntitlements) Error: \(String(describing: error))");
            }
            
            if(result == nil) {
                result = BillingServicesRestorePurchasesResult.init(transactions: nil)
            }

            
            self.delegate?.didRestorePurchasesComplete?(result: result!, error: error as NSError?)
        }
    }
    
    //MARK: Transaction Management
    public func getUnfinishedTransactions() -> Array<BillingTransaction> {
        
        let rawTransactions = Transaction.unfinished
        var transactions: Array<BillingTransaction> = []
        
        Task.runSyncronously {
            transactions.append(contentsOf: await BillingTransaction.from(rawTransactions))
        }
        
        return transactions
    }
    
    public func tryClearingUnfinishedTransactions() {
        
        Task {
            let transactions = getUnfinishedTransactions()
            
            for eachTransaction in transactions {
                deliverEntitlementForTransaction(transaction: eachTransaction)
            }
        }
    }

    public func finishTransactions(_ transactions: Array<BillingTransaction>) {
        
        for transaction in transactions {
            transaction.finish()
        }
    }
        
    //MARK: - Helpers
    public func getProductWithId(_ id: String) -> BillingProduct? {
        if(isStoreInitialized()) {
            let product = products?.first(where: {$0.identifier == id})
            return product
        } else {
            return nil;
        }
    }
    
    //MARK: - Private Functions
    private func initializeStoreAsync(_ productDefinitions: Array<BillingProductDefinition>) async {
        let productIds = productDefinitions.map( { $0.identifier })
        
        var error: BillingServicesError?
        
        do {
            
            rawProducts = try await Product.products(for: productIds)
            await updateCurrentPurchases()
            products    = await rawProducts.asyncMap({await BillingProduct.from($0)})
        } catch let storeKitError as StoreKitError {
            error = BillingServicesError.initializeStoreFailed(code: BillingServicesErrorCode.from(storeKitError), reason: storeKitError.localizedDescription)
        } catch let localError {
            error = BillingServicesError.initializeStoreFailed(code: BillingServicesErrorCode.unknown, reason: localError.localizedDescription)
        }
        
        if (self.delegate != nil) {
            let invalidProductDefinitions = getInvalidProductDefinitions(productDefinitions, products ?? [])
            self.delegate?.didInitializeStoreComplete?(result: BillingServicesInitializeStoreResult(products: products ?? [], invalidProductDefinitions: invalidProductDefinitions), error: error as NSError?)
        }
    }
    
    private func getInvalidProductDefinitions(_ productDefinitions: Array<BillingProductDefinition>, _ products: Array<BillingProduct>) -> Array<BillingProductDefinition> {
        
        let receivedProductIds = products.map( {$0.identifier} )
        let invalidProductDefinitions = productDefinitions.filter( { !receivedProductIds.contains($0.identifier) })
        
        return invalidProductDefinitions
    }
    
    private func createTransactionUpdatesListenerTask() -> Task<Void, Never> {
            Task(priority: .background) {
                for await verificationResult in Transaction.updates {
                    await self.processTransaction(verificationResult)
                }
            }
    }
    
    private func buyProductAsync(_ product: Product, _ options: BuyProductOptions = BuyProductOptions()) async {
        var error: BillingServicesError?
        
        if(!isStoreInitialized()) {
            error = BillingServicesError.buyProductFailed(code: BillingServicesErrorCode.storeNotInitialized, reason: "Store not initialized. Make sure you initialize store before accessing any other api.")
        } else if (isProductPurchased(product.id)) {
            error = BillingServicesError.buyProductFailed(code: BillingServicesErrorCode.productOwned, reason: "Product was already owned by the user")
        } else {
            
            do {
                let purchaseResult = try await product.purchase(options: options.convert())
                
                switch purchaseResult {
                case .success(let verificationResult):
                    await processTransaction(verificationResult)
                case .pending:
                    //Do nothing
                    print("Transaction is currently in pending state...")
                    break
                case .userCancelled:
                    error = BillingServicesError.buyProductFailed(code: BillingServicesErrorCode.userCancelled, reason: "User cancelled the purchase.");
                    break
                @unknown default:
                    break
                }
                
            } catch let storeKitError as StoreKitError {
                error = BillingServicesError.buyProductFailed(code: BillingServicesErrorCode.from(storeKitError), reason: storeKitError.localizedDescription)
            } catch let purchaseError as Product.PurchaseError {
                error = BillingServicesError.buyProductFailed(code: BillingServicesErrorCode.from(purchaseError), reason: purchaseError.localizedDescription)
            } catch let localError {
                error = BillingServicesError.buyProductFailed(code: BillingServicesErrorCode.unknown, reason: localError.localizedDescription)
            }
        }
        
        if (self.delegate != nil && error != nil) {
            self.delegate?.didTransactionStateChange?(result: BillingServicesTransactionStateChangeResult(BillingTransaction.fromError(product, options, error! as NSError)))
        }
    }
    
    private func updateCurrentPurchases() async {
        let currentPurchasedTransactions = await BillingTransaction.from(Transaction.currentEntitlements)
        currentPurchases = currentPurchasedTransactions.map({$0.productIdentifier})
    }
    
    private func processTransaction(_ verificationResult: VerificationResult<Transaction>) async {
        
        guard case .verified(let rawTransaction) = verificationResult else {
            if case .unverified(_, let verificationError) = verificationResult {
                    Self.logger.error("Verification failed for the transaction with error: \(verificationError). Ignoring this transaction.")
                } else {
                    Self.logger.warning("Case not handled : \(verificationResult.debugDescription)");
                }
                return
        }
        
        
        //Try updating the purchases
        await updateCurrentPurchases()
    
        
        let billingTransaction = await BillingTransaction.from(rawTransaction, verificationResult.jwsRepresentation)
        deliverEntitlementForTransaction(transaction: billingTransaction)
    }
    
    private func deliverEntitlementForTransaction(transaction: BillingTransaction) {
        
        self.delegate?.didTransactionStateChange?(result: BillingServicesTransactionStateChangeResult(transaction))
        
        if(self.settings.autoFinishTransactions) {
            transaction.finish()
        }
    }
    
    private func isStoreInitialized() -> Bool {
        
        if(products == nil) {
            Self.logger.error("Store not initialized. Make sure you initialize store before accessing any other api.")
        }
        
        return products != nil
    }
    
    private func createPurchaseIntentsListenerTask() -> Task<Void, Never> {
            Task(priority: .background) {
#if !os(tvOS)
                if #available(iOS 16.4, *) {
                    
                    for await purchaseIntent in PurchaseIntent.intents {
                        Self.logger.info("[Appstore Promotion] Received event for purchase intent.");
                        await waitUntilStoreIsInitialised()

                        Self.logger.info("[Appstore Promotion] Prompting for purchase.");
                        //But queue it and trigger after initialisation
                        await buyProductAsync(purchaseIntent.product)
                    }
                } else  {
                    // Fallback on earlier versions
                    Self.logger.warning("Not possible to capture purchase intents on older versions!")
                }
#else
                Self.logger.info("Purchase intents not supported on tvOS.")
#endif
            }
    }
    
    private func waitUntilStoreIsInitialised() async {
        while products == nil {
            try? await Task.sleep(nanoseconds: 100_000_000)
        }
    }
}
