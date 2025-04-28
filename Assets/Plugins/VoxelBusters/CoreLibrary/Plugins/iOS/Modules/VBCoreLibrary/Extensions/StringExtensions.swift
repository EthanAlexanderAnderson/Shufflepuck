//
//  StringExtensions.swift
//  VBCoreLibrary
//
//  Created by Ayyappa on 28/03/24.
//

import Foundation

extension String? {
    public func isNullOrEmpty() -> Bool {
        
        guard let string = self else {
            return true
        }

        return string.isEmpty
    }
}
