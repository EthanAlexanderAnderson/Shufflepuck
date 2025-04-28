//
//  BillingProductOfferPaymentMode+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 05/04/24.
//

import Foundation
import StoreKit


extension BillingProductOfferPaymentMode {
    
    public static func from(_ rawPaymentMode: Product.SubscriptionOffer.PaymentMode) -> BillingProductOfferPaymentMode {
        switch rawPaymentMode {
            case .freeTrial:
                return .freeTrial
            case .payAsYouGo:
                return .payAsYouGo
            case .payUpFront:
                return .payUpFront
            default:
                print("Un-implemented for payment mode value: \(rawPaymentMode)");
                return .unknown
        }
    }
}
