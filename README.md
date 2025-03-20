# AI

Simple console app with a prompt to parse the provided json file using AWS Bedrock (Cohere).
You will need a default aws profile created to run this app

The input format has quite some fields, but only the following are read

```json
{
  "ProgramName": "Dupixent MyWay Copay Card",
  "CoverageEligibilities": [
    "Commercially insured"
  ],
  "EnrollmentURL": "https://www.dupixent.com/support-savings/copay-card",
  "EligibilityDetails": "- Patient must have commercial insurance, including health insurance exchanges, federal employee plans, or state employee plans\n- Not valid for prescriptions paid, in whole or in part, by Medicaid, Medicare, VA, DOD, TRICARE, or other federal or state programs including any state pharmaceutical assistance programs\n- Program offer is not valid for cash-paying patients\n- Patient must be prescribed the Program Product for an FDA-approved indication\n- Patient must be a legal resident of the US or a US territory\n- Patients residing in or receiving treatment in certain states may not be eligible",
  "IncomeReq": false,
  "IncomeDetails": "Data Not Available",
  "AddRenewalDetails": "Patient will be automatically re-enrolled every January 1st provided that their card has been used within 18 months",
  "ProgramDetails": "-  Eligible patients may pay as little as $0 for every month of Dupixent\n-  The maximum annual patient benefit under the Dupixent MyWay Copay Card Program is $13,000\n-  Patient will receive copay card information via email following online enrollment & eligibility questions\n-  Ongoing follow-up and education are provided by the Nurse Educator to help patients stay on track with DUPIXENT\n-  Patient will be automatically re-enrolled every January 1st provided that their card has been used within 18 months\n-  For assistance or additional information, call 844-387-4936, option 1, Monday-Friday, 8 am-9 pm ET\n-  Pharmacists: for questions, call the LoyaltyScript program at 855-520-3765 (8am-8pm EST, Monday-Friday)",
  "AssistanceType": "Coupon",
  "FundLevelType": null
}
```

It outputs the following json

```json
 {
  "program_name": "Dupixent MyWay Copay Card",
  "coverage_eligibilities": [
    "Commercially insured"
  ],
  "program_type": "Coupon",
  "requirements": [
    {
      "name": "insurance",
      "value": [
        "commercial"
      ],
      "op": "All"
    },
    {
      "name": "payment_source",
      "value": [
        "Medicaid",
        "Medicare",
        "VA",
        "DOD",
        "TRICARE",
        "federal",
        "state"
      ],
      "op": "NotIn"
    },
    {
      "name": "eligibility_for_program",
      "value": true,
      "op": "Equal"
    }
  ],
  "benefits": [
    {
      "name": "monthly_copay",
      "value": 0
    },
    {
      "name": "annual_benefit_cap",
      "value": 13000
    }
  ],
  "forms": [
    {
      "name": "Enrollment Form",
      "link": "https://www.dupixent.com/support-savings/copay-card"
    }
  ],
  "funding": {
    "evergreen": true,
    "current_funding_level": "Data Not Available"
  },
  "details": [
    {
      "eligibility": "Must be a legal resident of the US or a US territory",
      "program": "Copay Card Benefits",
      "renewal": "Patient will be automatically re-enrolled every January 1st provided that their card has been used within 18 months",
      "income": "Not required"
    }
  ]
}
```

