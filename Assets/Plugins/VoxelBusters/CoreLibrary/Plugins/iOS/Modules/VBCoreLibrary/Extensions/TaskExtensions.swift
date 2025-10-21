//
//  TaskExtensions.swift
//  VBCoreLibrary
//
//  Created by Ayyappa on 28/03/24.
//

import Foundation

// Ref: https://stackoverflow.com/a/75818753/413306
extension Task where Failure == Error {
    
    public static func runSyncronously(operation: @escaping () async throws -> Success) {
        let semaphore = DispatchSemaphore(value: 0)

        Task(priority: TaskPriority.high) {
            defer { semaphore.signal() } //This makes sure the signal call is triggered irrespective of the operation fails or not. Similar to finally block
            return try await operation()
            
        }

        semaphore.wait()
    }
}
