﻿using Stratis.SmartContracts;
using Stratis.SmartContracts.Exceptions;

public sealed class ThrowRefundGasExceptionContract : SmartContract
{
    public ThrowRefundGasExceptionContract(SmartContractState state)
        : base(state)
    {
    }

    public void ThrowException()
    {
        SpendGas(10);

        throw new RefundGasException();
    }
}