//
//  BillingServicesSettings.swift
//  BillingServices
//
//  Created by Ayyappa on 27/03/24.
//

import Foundation

@objcMembers
public class BillingServicesSettings : NSObject {
    public private(set) var autoFinishTransactions: Bool = true
    
    public init(autoFinishTransactions: Bool) {
        self.autoFinishTransactions = autoFinishTransactions
    }
}
