//
//  SequenceExtensions.swift
//  VBCoreLibrary
//
//  Created by Ayyappa on 05/04/24.
//

import Foundation

// Taken from: https://gist.github.com/MahdiBM/57f4aada183326a6679a76bff9fef8eb
extension Sequence {
    public func asyncMap<T>(
        _ closure: @Sendable (Element) async throws -> T
    ) async rethrows -> [T] {
        var array: [T] = []
        array.reserveCapacity(self.underestimatedCount)
        for element in self {
            array.append(try await closure(element))
        }
        return array
    }
}
