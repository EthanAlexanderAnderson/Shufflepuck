//
//  BillingProductDefinition.swift
//  BillingServices
//
//  Created by Ayyappa on 26/03/24.
//

import Foundation

@objcMembers
public class BillingProductDefinition : NSObject {
    
    public private(set) var identifier: String;
    
    public init(identifier: String) {
        self.identifier = identifier;
    }
    
}
