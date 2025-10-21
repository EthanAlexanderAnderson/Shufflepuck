//
//  BillingProductExtensions.swift
//  BillingServices
//
//  Created by Ayyappa on 26/03/24.
//

import Foundation
import StoreKit

extension BillingProduct {

    public static func from(_ rawProduct: Product) async -> BillingProduct {
        
        let subscriptionInfo = await BillingProductSubscriptionInfo.from(rawProduct)
        let offers = getOffersArray(rawProduct, subscriptionInfo?.isEligibleForIntroductoryOffer ?? false);
        
        return BillingProduct(  identifier: rawProduct.id,
                                localizedTitle: rawProduct.displayName,
                                localizedDescription: rawProduct.description,
                                price: BillingPrice(Double(truncating: rawProduct.price.round(fractionDigits: 2) as NSNumber), rawProduct.priceFormatStyle.currencyCode, rawProduct.priceFormatStyle.locale.currencySymbol ?? "", rawProduct.displayPrice),
                                subscriptionInfo: subscriptionInfo,
                                offers: offers)
        
    }
    
    private static func getOffersArray(_ rawProduct: Product,_ isEligibleForIntroOffer: Bool) -> Array<BillingProductOffer> {
        var array: Array<BillingProductOffer> = Array()
        
        if(rawProduct.subscription == nil) {
            return array;
        }
        
        if(isEligibleForIntroOffer && rawProduct.subscription?.introductoryOffer != nil) {
            array.append(BillingProductOffer.from(rawProduct.subscription?.introductoryOffer, rawProduct.priceFormatStyle)!)
        } else if (!isEligibleForIntroOffer && rawProduct.subscription?.introductoryOffer != nil) {
            print("Check: When not eligible for intro offer, intro offer is being sent from apple!");
        }

        array.append(contentsOf: BillingProductOffer.from(rawProduct.subscription!.promotionalOffers, rawProduct.priceFormatStyle))
        
        return array
    }
}
