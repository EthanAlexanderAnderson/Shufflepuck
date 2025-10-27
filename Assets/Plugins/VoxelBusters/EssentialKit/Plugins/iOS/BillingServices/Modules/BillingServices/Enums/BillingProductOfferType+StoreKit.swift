//
//  BillingProductOfferType+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 05/04/24.
//

import Foundation
import StoreKit

extension BillingProductOfferCategory {
    
    public static func from(_ rawOfferType: Product.SubscriptionOffer.OfferType) -> BillingProductOfferCategory {
        
        switch rawOfferType {
            case .introductory:
                return BillingProductOfferCategory.introductory
            case .promotional:
                return BillingProductOfferCategory.promotional
                
            default:
                print("Un-implemented raw offer type value: \(rawOfferType)");
                return BillingProductOfferCategory.unknown
        }
        
    }
    
    public static func from(_ optionalRawOfferType: Transaction.OfferType?) -> BillingProductOfferCategory {
        
        guard let rawOfferType = optionalRawOfferType else {
            return BillingProductOfferCategory.unknown
        }
        
        switch rawOfferType {
            case .introductory:
                return BillingProductOfferCategory.introductory
            case .promotional:
                return BillingProductOfferCategory.promotional
                
            default:
                print("Un-implemented raw offer type value: \(rawOfferType)");
                return BillingProductOfferCategory.unknown
        }
        
    }
    
}
