//
//  BillingProductType.swift
//  BillingServices
//
//  Created by Ayyappa on 04/04/24.
//

import Foundation

@objc
public enum BillingProductType: Int {
    case consumable
    case nonConsumable
    case nonRenewable
    case autoRenewable
}
