//
//  BillingPeriod.swift
//  BillingServices
//
//  Created by Ayyappa on 04/04/24.
//

import Foundation

@objcMembers
public class BillingPeriod: NSObject {
    
    public private(set) var duration: Double;
    public private(set) var unit: BillingPeriodUnit;
    
    internal init(duration: Double, unit: BillingPeriodUnit) {
        self.duration = duration
        self.unit = unit
    }
}
