//
//  BillingServicesError.swift
//  BillingServices
//
//  Created by Ayyappa on 24/03/24.
//

import Foundation

public enum BillingServicesError: CustomNSError, Error {
    case initializeStoreFailed(code: BillingServicesErrorCode, reason: String)
    case buyProductFailed(code: BillingServicesErrorCode, reason: String)
    case restorePurchasesFailed(code: BillingServicesErrorCode, reason: String)
    
    public static var errorDomain: String { NSString(#fileID).pathComponents.first! }
    
    public var errorCode: Int {
        switch self {
            case    .buyProductFailed(let code, _),
                    .initializeStoreFailed(let code, _),
                    .restorePurchasesFailed(let code, _):
                    return code.rawValue
        }
    }
    
    public var errorUserInfo: [String : Any] {
        var description: String
        
        switch self {
            case    .buyProductFailed(_, let reason),
                    .initializeStoreFailed(_, let reason),
                    .restorePurchasesFailed(_,let reason):
                    description = reason
        }
        
        return [NSLocalizedDescriptionKey: description];
    }
}
