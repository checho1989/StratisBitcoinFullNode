﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Stratis.SmartContracts;
using Stratis.SmartContracts.Backend;
using Stratis.SmartContracts.ContractValidation;
using Stratis.SmartContracts.State;
using Stratis.SmartContracts.Util;
using NBitcoin;
using Xunit;

namespace Stratis.Bitcoin.Features.SmartContracts.Tests
{
    public class ReflectionVirtualMachineTests
    {
        private SmartContractDecompiler decompiler;
        private SmartContractGasInjector gasInjector;

        public ReflectionVirtualMachineTests()
        {
            this.decompiler = new SmartContractDecompiler();
            this.gasInjector = new SmartContractGasInjector();
        }

        [Fact]
        public void ContractRunTest()
        {
            ulong blockNum = 1;
            uint160 callerAddress = new uint160(1);
            ulong callValue = 0;
            uint160 coinbaseAddress = new uint160(2);
            uint160 contractAddress = new uint160(3);
            ulong difficulty = 0;
            ulong gasLimit = 500000;
            ulong gasPrice = 1;
            
            // Note that this is skipping validation and when on-chain, 
            byte[] contractCode = GetFileDllHelper.GetAssemblyBytesFromFile("SmartContracts/StorageTest.cs");
            SmartContractDecompilation decomp = this.decompiler.GetModuleDefinition(contractCode);
            this.gasInjector.AddGasCalculationToContract(decomp.ContractType, decomp.BaseType);
            MemoryStream mem = new MemoryStream();
            decomp.ModuleDefinition.Write(mem);
            byte[] adjustedContractCode = mem.ToArray();
            ISource<byte[], byte[]> stateDB = new NoDeleteSource<byte[], byte[]>(new MemoryDictionarySource());
            ContractStateRepositoryRoot repository = new ContractStateRepositoryRoot(stateDB);
            IContractStateRepository track = repository.StartTracking();
            ReflectionVirtualMachine vm = new ReflectionVirtualMachine(repository);
            SmartContractExecutionContext context = new SmartContractExecutionContext
            {
                BlockNumber = blockNum,
                CallerAddress = callerAddress,
                CallValue = callValue,
                CoinbaseAddress = coinbaseAddress,
                ContractAddress = contractAddress,
                ContractMethod = "StoreData",
                ContractTypeName = "StorageTest",
                Difficulty = difficulty,
                GasLimit = gasLimit,
                GasPrice = gasPrice,
                Parameters = new object[] {}
            };
            SmartContractExecutionResult result = vm.ExecuteMethod(adjustedContractCode, context);
            track.Commit();

            Assert.Equal(Encoding.UTF8.GetBytes("TestValue"), track.GetStorageValue(contractAddress, Encoding.UTF8.GetBytes("TestKey")));
            Assert.Equal(Encoding.UTF8.GetBytes("TestValue"), repository.GetStorageValue(contractAddress, Encoding.UTF8.GetBytes("TestKey")));
        }
    }
}
