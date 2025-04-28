//
//  BillingProductOffer+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 05/04/24.
//

import Foundation
import StoreKit

extension BillingProductOffer {
    
    public static func from(_ optionalRawOffer: Product.SubscriptionOffer?, _ priceFormatStyle: Decimal.FormatStyle.Currency) -> BillingProductOffer? {
        
        guard let rawOffer = optionalRawOffer else {
            return nil
        }
        
        let offer = BillingProductOffer(identifier: rawOffer.id ?? "intro",
                                                    category: BillingProductOfferCategory.from(rawOffer.type),
                                                    pricingPhases: getPricingPhases(rawOffer, priceFormatStyle)
                                        )
        
        return offer
    }
    
    public static func from(_ rawOffers: [Product.SubscriptionOffer],_ priceFormatStyle: Decimal.FormatStyle.Currency) -> [BillingProductOffer] {
        
        var offers: [BillingProductOffer] = []
        
        for eachRawOffer in rawOffers {
            offers.append(BillingProductOffer.from(eachRawOffer, priceFormatStyle)!)
        }
        
        return offers
    }
    
    private static func getPricingPhases(_ rawOffer: Product.SubscriptionOffer, _ priceFormatStyle: Decimal.FormatStyle.Currency) -> Array<BillingProductOfferPricingPhase> {
        var pricingPhases: Array<BillingProductOfferPricingPhase> = []
        
        print("Offer period: " + rawOffer.period.debugDescription);
        //There will be only one pricing phase for iOS per offer.
        let pricingPhase: BillingProductOfferPricingPhase = BillingProductOfferPricingPhase(paymentMode: BillingProductOfferPaymentMode.from(rawOffer.paymentMode),
                                                                                            price: BillingPrice(Double(truncating: rawOffer.price as NSNumber), priceFormatStyle.currencyCode, priceFormatStyle.locale.currencySymbol ?? "", rawOffer.displayPrice ),
                                                                                            period: BillingPeriod.from(rawOffer.period),
                                                                                            repeatCount: rawOffer.periodCount)
        
        pricingPhases.append(pricingPhase)
        
        return pricingPhases
    }
}

