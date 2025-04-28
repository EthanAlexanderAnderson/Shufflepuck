//
//  BillingServicesErrorCode+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 23/05/24.
//

import Foundation
import StoreKit


extension BillingServicesErrorCode {
    static func from(_ error: StoreKitError) -> BillingServicesErrorCode {
        switch error {
        case StoreKitError.unknown:
            return BillingServicesErrorCode.unknown
            
        case StoreKitError.networkError:
            return BillingServicesErrorCode.networkError
        
        case StoreKitError.systemError:
            return BillingServicesErrorCode.systemError
        
        case StoreKitError.userCancelled:
            return BillingServicesErrorCode.userCancelled
            
        case StoreKitError.notAvailableInStorefront:
            return BillingServicesErrorCode.productNotAvailable
            
        case StoreKitError.notEntitled:
            return BillingServicesErrorCode.billingNotAvailable
            
        default:
            print("\(error) code not implemented! Report to plugin developer.")
            return BillingServicesErrorCode.unknown
        }
    }
    
    static func from(_ error: Product.PurchaseError) -> BillingServicesErrorCode {
        switch error {
        case Product.PurchaseError.ineligibleForOffer:
            return BillingServicesErrorCode.offerNotApplicable
            
        case Product.PurchaseError.invalidOfferPrice,
            Product.PurchaseError.invalidOfferIdentifier,
            Product.PurchaseError.invalidOfferSignature,
            Product.PurchaseError.missingOfferParameters:
            return BillingServicesErrorCode.offerNotValid
        
        case Product.PurchaseError.invalidQuantity:
            return BillingServicesErrorCode.quantityNotValid
            
        case Product.PurchaseError.productUnavailable:
            return BillingServicesErrorCode.productNotAvailable
        
        case Product.PurchaseError.purchaseNotAllowed:
            return BillingServicesErrorCode.billingNotAvailable
            
        default:
            print("\(error) code not implemented!  Report to plugin developer.")
            return BillingServicesErrorCode.unknown
        }
    }
}
