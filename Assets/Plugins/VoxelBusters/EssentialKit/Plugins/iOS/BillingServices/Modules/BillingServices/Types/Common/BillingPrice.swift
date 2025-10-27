//
//  BillingPrice.swift
//  BillingServices
//
//  Created by Ayyappa on 03/05/24.
//

import Foundation

@objcMembers
public class BillingPrice: NSObject {
    
    public let value: Double;
    public let currencyCode: String;
    public let currencySymbol: String;
    public let localizedDisplay: String;
    
    internal init(_ value: Double, _ currencyCode: String, _ currencySymbol: String, _ localizedDisplay: String) {
        self.value = value
        self.currencyCode = currencyCode
        self.currencySymbol = currencySymbol
        self.localizedDisplay = localizedDisplay
    }
    
}
