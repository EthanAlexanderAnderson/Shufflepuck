//
//  BillingProductOffer.swift
//  BillingServices
//
//  Created by Ayyappa on 04/04/24.
//

import Foundation

@objcMembers
public class BillingProductOffer: NSObject {

    public private(set) var identifier: String
    public private(set) var category: BillingProductOfferCategory
    public private(set) var pricingPhases: Array<BillingProductOfferPricingPhase>

    internal init(identifier: String, category: BillingProductOfferCategory, pricingPhases: Array<BillingProductOfferPricingPhase>) {
        self.identifier = identifier
        self.category = category
        self.pricingPhases = pricingPhases
    }
}
