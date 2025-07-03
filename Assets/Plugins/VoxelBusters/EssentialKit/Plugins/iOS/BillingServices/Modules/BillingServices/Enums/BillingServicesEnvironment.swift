//
//  BillingServicesServerEnvironment.swift
//  BillingServices
//
//  Created by Ayyappa on 04/04/24.
//

import Foundation

@objc
public enum BillingServicesEnvironment: Int {
    case unknown
    case production
    case sandbox
    case local
}
