//
//  BillingProductPromoOffer.swift
//  BillingServices
//
//  Created by Ayyappa on 27/03/24.
//

import Foundation

@objcMembers
public class BillingProductOfferRedeemDetails : NSObject {
    public private(set) var offerId: String
    public private(set) var keyId: String
    public private(set) var nonce: UUID
    public private(set) var signature: Data
    public private(set) var timestamp: Int
    
    public init(offerId: String, keyId: String, nonce: UUID, signature: Data, timestamp: Int) {
        self.offerId = offerId
        self.keyId = keyId
        self.nonce = nonce
        self.signature = signature
        self.timestamp = timestamp
    }
}
