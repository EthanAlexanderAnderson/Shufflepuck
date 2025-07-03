//
//  BillingServicesErrorCode.swift
//  BillingServices
//
//  Created by Ayyappa on 25/03/24.
//

import Foundation
import StoreKit

@objc public enum BillingServicesErrorCode: Int {
    case unknown
    case networkError
    case systemError
    case billingNotAvailable
    case storeNotInitialized
    case storeIsBusy
    case userCancelled
    case offerNotApplicable
    case offerNotValid
    case quantityNotValid
    case productNotAvailable
    case productOwned
    case featureNotAvailable
}

