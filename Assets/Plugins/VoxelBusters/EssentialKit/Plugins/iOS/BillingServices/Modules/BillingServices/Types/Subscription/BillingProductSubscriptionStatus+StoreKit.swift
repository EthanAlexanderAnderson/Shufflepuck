//
//  BillingProductSubscriptionStatus+StoreKit.swift
//  BillingServices
//
//  Created by Ayyappa on 05/04/24.
//

import Foundation
import StoreKit

extension BillingProductSubscriptionStatus {
    
    public static func from(_ rawTransaction: Transaction) async -> BillingProductSubscriptionStatus? {
        
        guard let rawSubscriptionStatus = await rawTransaction.subscriptionStatus else {
            return nil
        }

        var renewalInfo: BillingProductSubscriptionRenewalInfo?;
        
        switch rawSubscriptionStatus.renewalInfo {
            case .verified(let rawRenewalInfo):
                renewalInfo = BillingProductSubscriptionRenewalInfo.from(String(rawTransaction.id), rawTransaction.purchaseDate, rawSubscriptionStatus.state, rawRenewalInfo)
            default:
                renewalInfo = nil
        }
        
        var offerId: String? = nil;
        var offerType: Transaction.OfferType? = nil;
        
        if #available(iOS 17.2, *) {
            offerId = rawTransaction.offer?.id
            offerType = rawTransaction.offer?.type;
        }
                
        
        let status: BillingProductSubscriptionStatus = BillingProductSubscriptionStatus(
            groupIdentifier: rawTransaction.subscriptionGroupID!,
            renewalInfo: renewalInfo,
            expirationDate: rawTransaction.expirationDate,
            isUpgraded: rawTransaction.isUpgraded,
            appliedOfferIdentifier: offerId,
            appliedOfferType: BillingProductOfferCategory.from(offerType)
        );

        return status
    }
    
}
