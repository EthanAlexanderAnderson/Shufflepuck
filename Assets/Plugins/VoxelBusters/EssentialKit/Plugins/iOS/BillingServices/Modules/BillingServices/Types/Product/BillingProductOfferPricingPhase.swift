//
//  BillingProductOfferPricingPhase.swift
//  BillingServices
//
//  Created by Ayyappa on 03/05/24.
//

import Foundation

@objcMembers
public class BillingProductOfferPricingPhase: NSObject {
    
    public let paymentMode: BillingProductOfferPaymentMode
    public let price: BillingPrice
    public let period: BillingPeriod
    public let repeatCount: Int
    
    internal init(paymentMode: BillingProductOfferPaymentMode, price: BillingPrice, period: BillingPeriod, repeatCount: Int) {
        
        self.paymentMode = paymentMode
        self.price = price
        self.period = period
        self.repeatCount = repeatCount
    }
}
