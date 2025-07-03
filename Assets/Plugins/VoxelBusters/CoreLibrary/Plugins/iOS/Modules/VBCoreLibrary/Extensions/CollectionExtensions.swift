//
//  CollectionExtensions.swift
//  VBCoreLibrary
//
//  Created by Ayyappa on 22/05/24.
//

import Foundation

extension Dictionary {
    public func toJson() -> String? {
        guard let data = try? JSONSerialization.data(withJSONObject: self,
                                                            options: [.prettyPrinted]) else {
            return nil
        }

        return String(data: data, encoding: .utf8)
    }
}
