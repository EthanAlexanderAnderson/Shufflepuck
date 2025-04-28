//
//  BuyProductOptions.swift
//  BillingServices
//
//  Created by Ayyappa on 26/03/24.
//

import Foundation
import StoreKit

@objcMembers
public class BuyProductOptions : NSObject  {
    public private(set) var quantity: Int   = 1
    public private(set) var tag: UUID?
    public private(set) var offerRedeemDetails: BillingProductOfferRedeemDetails?
    
    public init(quantity: Int = 1, tag: UUID? = nil, offerRedeemDetails: BillingProductOfferRedeemDetails? = nil) {
        self.quantity = quantity
        self.tag = tag
        self.offerRedeemDetails = offerRedeemDetails
    }
}

extension BuyProductOptions {
    public func convert() -> Set<Product.PurchaseOption> {
        var purchaseOptions = Set<Product.PurchaseOption>()
        
        if let accountToken = self.tag {
            purchaseOptions.insert(Product.PurchaseOption.appAccountToken(accountToken))
        }

        if let promotionalOffer = self.offerRedeemDetails {
            purchaseOptions.insert(Product.PurchaseOption.promotionalOffer(offerID: promotionalOffer.offerId, keyID: promotionalOffer.keyId, nonce: promotionalOffer.nonce, signature: promotionalOffer.signature, timestamp: promotionalOffer.timestamp))
        }

        purchaseOptions.insert(Product.PurchaseOption.quantity(quantity))
        
        return purchaseOptions
    }
}
