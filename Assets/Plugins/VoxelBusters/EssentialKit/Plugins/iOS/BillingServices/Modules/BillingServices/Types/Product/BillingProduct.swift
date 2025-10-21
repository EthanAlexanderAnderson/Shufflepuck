//
//  BillingProduct.swift
//  BillingServices
//
//  Created by Ayyappa on 26/03/24.
//

import Foundation

@objcMembers
public class BillingProduct : NSObject  {
    
    public private(set) var identifier: String
    public private(set) var localizedTitle: String
    public private(set) var localizedDescription: String
    public private(set) var price: BillingPrice
    public private(set) var subscriptionInfo: BillingProductSubscriptionInfo?
    public private(set) var offers: Array<BillingProductOffer>
    
    internal init(identifier: String, localizedTitle: String, localizedDescription: String, price: BillingPrice, subscriptionInfo: BillingProductSubscriptionInfo? = nil, offers: Array<BillingProductOffer>) {
        self.identifier = identifier
        self.localizedTitle = localizedTitle
        self.localizedDescription = localizedDescription
        self.price = price
        self.subscriptionInfo = subscriptionInfo
        self.offers = offers;
    }
}
