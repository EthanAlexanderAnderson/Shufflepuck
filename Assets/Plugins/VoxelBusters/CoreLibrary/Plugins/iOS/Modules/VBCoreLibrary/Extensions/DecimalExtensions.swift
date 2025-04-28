//
//  DecimalExtensions.swift
//  VBCoreLibrary
//
//  Created by Ayyappa Reddy Janga on 10/05/24.
//

import Foundation

extension Decimal {
    public func round(fractionDigits: Int) -> Decimal {
        
        var result = Decimal()
        var value = self

        NSDecimalRound(&result, &value, fractionDigits, NSDecimalNumber.RoundingMode.plain)
        return result
    }
}
